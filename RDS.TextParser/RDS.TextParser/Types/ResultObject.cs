namespace RDS.TextParser.Types
{
    public class ResultObject<T>
    {
        public bool Ok { get; set; }

        public T Result { get; set; }
    }
}
