using Dungeons_and_Dragons;
using PainterTool;
using PainterTool.Examples;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using RayFire;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace QuizCanners.IsItGame.Develop
{
    [Serializable]
    public class PlayerGun_RocketLauncher : IPEGI
    {

        [SerializeField] private RayfireGun gun;
        public PlaytimePainter_BrushConfigScriptableObject brushConfig;
        public PlaytimePainter_BrushConfigScriptableObject fleshImpactBrushConfig;
        public SavingThrow savingThrow = new() { DC = 17, Score = AbilityScore.Dexterity };
        public Damage Damage = new() { DamageDice = new List<Dice> { Dice.D10 }, DamageType = DamageType.Fire };
        [SerializeField] private float _pushForce = 200;
        [SerializeField] private float _explosionRadius = 8;

        private bool TryDestroyMonsters(Vector3 origin) 
        {
            bool splatterMonsters = false;

            int maxPaints = 2;

            float GetPushForce(float distance) => 2 * (_explosionRadius - distance) / _explosionRadius;

            var deadOnes = Singleton.Get<Pool_MonstersDetailed>();

            if (deadOnes) 
            {
                foreach(var monster in deadOnes) 
                {
                    if (monster.IsAlive == false) 
                    {
                        var pos = monster.GetActivePosition();
                        var pushDirection = pos - origin;
                        var dist = pushDirection.magnitude;
                        if (dist < _explosionRadius)
                        {
                            monster.Giblets(pushVector: pushDirection.normalized, pushForce01: GetPushForce(dist));
                        }
                    }
                }
            }

            var enm = Singleton.Get<Pool_Monsters_Data>();

            if (enm)
            {
                foreach (C_Monster_Data monsterData in enm)
                {
                    var pos = monsterData.GetActivePosition();
                    var pushDirection = pos - origin;
                    var dist = pushDirection.magnitude;

                   

                    if (dist < _explosionRadius)
                    {
                        var proxy_detailed = monsterData.proxy_detailed;

                        if (monsterData.IsAlive)
                        {
                            if (dist < _explosionRadius * 0.3f)
                            {
                                monsterData.Disintegrate(); // pushDirection.normalized, pushForce01: GetPushForce());

                                Singleton.Try<Pool_VolumetricBlood>(s => s.TrySpawnRandom(origin, pushDirection, out BFX_BloodController controller, size: 4));

                                Singleton.Try<Pool_VolumetricBlood>(s => s.TrySpawnRandom(origin, -pushDirection, out BFX_BloodController controller, size: 3));

                                splatterMonsters = true;
                            }
                            else
                            {
                                if (proxy_detailed)
                                {
                                    var randomLimb = proxy_detailed.GetRandomCollider();

                                    var fragmentOrigin = origin + _latestNormal * 0.2f;

                                    var ray = QcUnity.RaySegment(fragmentOrigin, randomLimb.transform.position, out float distance);// new Ray(fragmentOrigin, direction: randomLimb.transform.position);


                                    if (maxPaints > 0)
                                    {
                                        maxPaints--;

                                        Singleton.Try<Singleton_ChornobaivkaController>(s =>
                                        {
                                            if (s.CastHardSurface(ray, out var limbHit, maxDistance: distance))
                                            {
                                                C_MonsterDetailed hitMonster = limbHit.transform.GetComponentInParent<C_MonsterDetailed>();
                                                if (hitMonster && hitMonster == monsterData)
                                                {
                                                    PainDamage(limbHit, fleshImpactBrushConfig);
                                                }
                                            }
                                        });
                                    }

                                    proxy_detailed.DropRigid();
                                }
                                else if (monsterData.proxy_instanced)
                                    monsterData.proxy_instanced.Giblets(pushDirection.normalized, pushForce01: GetPushForce(dist));
                            }

                            monsterData.IsAlive = false;
                        }

                        // For dead ones
                        /* else if (dist < _explosionRadius * 0.6f)
                         {
                             if (proxy_detailed)
                             {
                                 if (proxy_detailed.LimbsState != C_MonsterEnemy.LimbsControllerState.Giblets && proxy_detailed.LimbsState != C_MonsterEnemy.LimbsControllerState.Disintegrating)
                                     proxy_detailed.Giblets(pushDirection.normalized, pushForce01: GetPushForce());
                             }

                             monsterData.IsAlive = false;
                         }
                          else if (monsterData.proxy_instanced)
                                 monsterData.proxy_instanced.Giblets(pushDirection.normalized, pushForce01: GetPushForce());
                         */

                        if (proxy_detailed && proxy_detailed.LimbsState == C_MonsterDetailed.LimbsControllerState.Ragdoll)
                        {
                            proxy_detailed.Push(force: _pushForce * 40, origin: origin - Vector3.up * 2, radius: _explosionRadius);
                        }
                    }
                }
            }

            return splatterMonsters;
        }

        public void Explosion(RaycastHit hit, Vector3 projectileVelocity, State state)
        {
            Vector3 origin = hit.point;
            _latestNormal = hit.normal;
           // int killedMonsters = 0;
            bool splatterMonsters = TryDestroyMonsters(origin);

            const float volume = 0.5f;

            Singleton.Try<Singleton_ZibraLiquidsBlood>(s => 
            {
                s.TryCreateExplosion(hit.point + Vector3.down * 1.5f);
            });

            if (splatterMonsters)
            {
                Game.Enums.UiSoundEffects.Explosion_Gory.PlayOneShotAt(hit.point, clipVolume: volume);
            }
            else
                Singleton.Try<Singleton_CameraOperatorGodMode>(
                    onFound: cam => {
                            if ((cam.transform.position - hit.point).magnitude < _explosionRadius)
                                Game.Enums.UiSoundEffects.Explosion_Near.PlayOneShotAt(hit.point, clipVolume: volume);
                            else 
                                Game.Enums.UiSoundEffects.Explosion.PlayOneShotAt(hit.point, clipVolume: volume);
                        }, 
                    onFailed: () => Game.Enums.UiSoundEffects.Explosion.PlayOneShotAt(hit.point, clipVolume: volume));


            if (!Camera.main.IsInCameraViewArea(hit.point))
                return;

            latestState = state;
            _explosionsLeft.ResetCountDown(10);
            _origin = origin;
            iteration = 0;

            if (state.Gun)
            {
                var cmp = hit.transform.gameObject.GetComponentInParent<C_RayFireRespawn>();
                if (cmp)
                    cmp.SetDamaged(true);

                var nrm = projectileVelocity.normalized;

                state.Gun.maxDistance = 10f;
                state.Gun.Shoot(origin - nrm, nrm);
            }

            Singleton.Try<Pool_ImpactLightsController>(s =>
            {
                if (s.TrySpawnIfVisible(origin, out var light))
                    light.SetSize(256);
            });

            if (Pool.TrySpawnIfVisible<C_SmokeEffectOnImpact>(origin, out var smoke))
                smoke.PlayAnimateFromDot(2f);

            Singleton.Try<Pool_PhisXDebrisParticles>(s =>
            {
                s.PushFromExplosion(hit.point, force: _pushForce, radius: 25);

                int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 30);

                var reflected = Vector3.Reflect(-projectileVelocity.normalized, hit.normal);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawnIfVisible(hit.point, out var debri))
                    {
                        var big = UnityEngine.Random.value;

                        debri.Reset(size: 0.02f + 0.5f * big * big);

                        var direction = Vector3.Lerp(reflected, hit.normal, 0.5f + big * 0.5f);

                        debri.Push(pushVector: direction, pushForce: 10.5f, pushRandomness: debri.Size * 15, torqueForce: 540, heat: big * 35);
                    }
                    else break;
                }
            });

            Singleton.Try<Pool_PhisXEmissiveParticles>(s =>
            {
                s.PushFromExplosion(hit.point, force: 5, 20);

                int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 10);

                var reflected = Vector3.Reflect(-projectileVelocity.normalized, hit.normal);

                for (int i = 0; i < count; i++)
                {
                    if (s.TrySpawnIfVisible(hit.point, out var debri))
                    {
                        var big = UnityEngine.Random.value;

                        debri.Size = (0.2f + big);

                        var direction = Vector3.Lerp(reflected, hit.normal, 0.5f + big * 0.5f);

                        debri.Push(pushVector: direction, pushForce: debri.Size * 40f, pushRandomness: 2f, torqueForce: 0);
                    }
                    else break;
                }
            });

            Singleton.Try<Singleton_SDFPhisics_TinyEcs>(s =>
            {
                s.World.CreateEntity().AddComponent((ref ParticlePhisics.UpwardImpulse e) => 
                { 
                    e.EnergyLeft = 10; 
                    e.Position = hit.point;
                    e.Direction = hit.normal;
                });
            });

            Singleton.Try<Pool_ECS_HeatSmoke>(p =>
            {
                for (int i = 0; i < 5; i++)
                {
                    if (!p.TrySpawn(hit.point + UnityEngine.Random.insideUnitCircle.ToVector3XZ() * 3.5f))
                        break;
                }
            });

            if (Pool.TrySpawn<C_SpriteAnimation>(hit.point, out var spriteAnimation))
                spriteAnimation.transform.localScale = spriteAnimation.transform.localScale * 5;

            PainDamage(hit, brushConfig);
        }

        private void PainDamage(RaycastHit hit, PlaytimePainter_BrushConfigScriptableObject brush) 
        {
            var receivers = hit.transform.GetComponentsInParent<C_PaintingReceiver>();

            if (receivers.Length > 0)
            {
                C_PaintingReceiver receiver = receivers.GetByHit(hit, out int subMesh);

                if (receiver.GetTexture() is RenderTexture)
                {
                    var stroke = receiver.CreateStroke(hit);
                    receiver.CreatePaintCommandFor(stroke, brush.brush, subMesh).Paint();
                }
            }
        }


        public override string ToString() => "Rocket Launcher";

        [SerializeField] private pegi.EnterExitContext context = new();
        public void Inspect()
        {
            using (context.StartContext())
            {
                "Direct Impact Brush".PegiLabel().Edit_Enter_Inspect(ref brushConfig).Nl();
                "Shrapnel Impact Brush".PegiLabel().Edit_Enter_Inspect(ref fleshImpactBrushConfig).Nl();
            }
        }

        private State latestState;
        private Vector3 _origin;
        private Vector3 _latestNormal;
        private readonly LogicWrappers.CountDown _explosionsLeft = new();
        private readonly Gate.UnityTimeScaled _explosionDynamics = new();
        private int iteration;

        public void UpdateExplosions() 
        {
            if (!_explosionsLeft.IsFinished && _explosionDynamics.TryUpdateIfTimePassed(0.05f)) 
            {
                iteration++;
                _explosionsLeft.RemoveOne();

                var rndPosition = UnityEngine.Random.insideUnitSphere;

                var dott = Vector3.Dot(rndPosition, _latestNormal);

                if (dott < 0)
                    rndPosition = -rndPosition;

                dott = Mathf.Abs(dott);

                var spawnOffset = rndPosition * dott * (1f + iteration) * 1.5f;
                var spawnDirection = spawnOffset.normalized;

                Singleton.Try<Singleton_ChornobaivkaController>(s => 
                { 
                    if (s.CastHardSurface(new Ray(_origin, spawnDirection), out var hit)) 
                    {
                        spawnOffset = spawnDirection * Mathf.Min(spawnOffset.magnitude, Vector3.Distance(hit.point, _origin));
                    }
                });


                
                Singleton.Try<Pool_PhisXDebrisParticles>(s =>
                {
                    if (s.TrySpawnIfVisible(_origin, out var debri))
                    {
                        var big = UnityEngine.Random.value;

                        debri.Reset(size: big * 0.5f);

                        debri.Push(pushVector: spawnDirection, pushForce: 10.5f, pushRandomness: debri.Size * 5, torqueForce: 540, heat: big * 35);
                    }
                    
                });

                Singleton.Try<Pool_AnimatedSmoke>(s =>
                {
                    if (!s.TrySpawn(_origin + spawnOffset, inst => inst.UpscaleBy((1 + dott) * 3)))
                        _explosionsLeft.Clear();
                  
                });

                int segments = Mathf.RoundToInt(spawnOffset.magnitude);

                for (int i = 0; i < segments - 1; i++)
                {
                    var segmentPos = _origin + spawnOffset * ((i + 1f) / segments);

                  //  Pool.TrySpawn<C_SpriteAnimationOneShot>(segmentPos, el => el.UpscaleBy(3f));

                    Pool.TrySpawn<C_ECS_HeatSmoke>(segmentPos, smoke =>
                    {
                        smoke.AddHeat(10);
                    });
                }

                TryDestroyMonsters(_origin + spawnOffset);

                if (latestState.Gun)
                {
                    if (Physics.Raycast(_origin, spawnDirection, out var hit, maxDistance: (iteration + 1) * 0.5f * _explosionRadius))
                    {
                        var cmp = hit.transform.gameObject.GetComponentInParent<C_RayFireRespawn>();

                        if (cmp)
                        {
                            cmp.SetDamaged(true);
                            latestState.Gun.maxDistance = spawnDirection.magnitude + 5f;
                            latestState.Gun.Shoot(_origin, spawnDirection);
                        }
                    }
                }



                /*
                Singleton.Try<Pool_PhisXEmissiveParticles>(s =>
                {

                    int count = 1 + Mathf.FloorToInt(s.VacancyPortion * 10);

         
                    for (int i = 0; i < count; i++)
                    {
                        if (s.TrySpawnIfVisible(_origin, out var debri))
                        {
                            var big = UnityEngine.Random.value;

                            debri.Size = (0.2f + big);


                            debri.Push(pushVector: Vector3.up, pushForce: debri.Size * 7f, pushRandomness: 2f, torqueForce: 0);
                        }
                        else break;
                    }
                });*/


            }
        }

        [Serializable]
        public class State : IPEGI
        {
            [SerializeField] public RayfireGun Gun;

            public void Inspect()
            {
                "Gun".PegiLabel(60).Edit(ref Gun).Nl();
            }
        }

    }
}
