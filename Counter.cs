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
            if (++_cur == 3)
            {
                Clear();
                return true;
            }
            return false;
        }

        public void Clear()
            => _cur = 0;

        public override string ToString()
        {
            return _cur.ToString();
        }
    }

    public class LawsCounter : Counter
    {
        public override bool Inc()
        {
            return ++_cur != 0;
        }
    }
}