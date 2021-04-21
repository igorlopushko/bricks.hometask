namespace Bricks.Hometask.Utility
{
    public class ConsoleLogger
    {
        private System.ConsoleColor _color;

        public ConsoleLogger(System.ConsoleColor color)
        {
            _color = color;
        }

        public void LogWriteLine(string text)
        {
            System.Console.ForegroundColor = _color;
            System.Console.WriteLine(text);
            System.Console.ResetColor();
        }

        public void LogWrite(string text)
        {
            System.Console.ForegroundColor = _color;
            System.Console.Write(text);
            System.Console.ResetColor();
        }
    }
}
