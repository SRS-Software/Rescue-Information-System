namespace RIS.Core.FireboardApi
{
    public class AuthKey
    {
        public AuthKey()
        {
        }

        public AuthKey(string key)
        {
            Key = key;
        }

        public string Key { get; set; }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Key)) return false;

                return true;
            }
        }

        public override string ToString()
        {
            return string.Format("AuthKey " + Key);
        }
    }
}