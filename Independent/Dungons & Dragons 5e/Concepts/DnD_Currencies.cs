using QuizCanners.Inspect;

namespace Dungeons_and_Dragons
{
    public enum Currency 
    {
        Copper = 0,
        Silver = 1,
        Electrum = 2,
        Gold = 3,
        Platinum = 4,
    }

    internal static class DnD_CurrencyExtensions 
    {
        
    }
 
    [System.Serializable]
    public class Wallet : IPEGI
    {
        [UnityEngine.SerializeField] private int[] _currencies;

        private void InitIfNull() 
        {
            if (_currencies == null || _currencies.Length != 5) 
            {
                _currencies = new int[5];
            }
        }

        public int this[Currency cur] 
        {
            get 
            {
                InitIfNull();
                return _currencies[(int)cur];
            }

            set 
            {
                InitIfNull();
                _currencies[(int)cur] = value;

            }
        }

        public void Inspect()
        {
            for (int i=0; i<5; i++) 
            {
                var enm = (Currency)i;
                var val = this[enm];

                if (i>0 && icon.Up.Click()) 
                {
                    
                }

                if (enm.ToString().editDelayed(ref val))
                    this[enm] = val;


                pegi.nl();
            }

        }

    }

}
