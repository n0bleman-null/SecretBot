namespace TelegramBot
{
    public abstract class Counter
    {
        protected uint _cur = 0;
        public uint Cur => _cur;
        public abstract bool Inc();
        
    }
    
    public class ElectionCounter : Counter
    {       
        public override bool Inc()
        {
            if (++_cur == Strategy.MaxElections)
            {
                return true;
            }
            return false;
        }

        public void Clear()
            => _cur = 0;
    }

    public class FascistLawsCounter : Counter
    {
        public override bool Inc()
        {
            return ++_cur != 0;
        }
    }
    
    public class LiberalLawsCounter : Counter
    {
        public override bool Inc()
        {
            return ++_cur != 0;
        }
    }
}