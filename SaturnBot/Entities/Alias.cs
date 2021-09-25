namespace SaturnBot.Entities
{
    public class CustomAlias
    {
        public string Alias;
        public string Command;

        public CustomAlias(string c, string a)
        {
            this.Command = c;
            this.Alias = a;
        }
    }
}
