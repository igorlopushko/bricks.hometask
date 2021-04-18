namespace Bricks.Hometask.Sandbox
{
    public class ConsoleLogger
    {
        private System.ConsoleColor _color;

        public ConsoleLogger(System.ConsoleColor color)
        {
            _color = color;
        }

        public void Log(string text)
        {
            System.Console.ForegroundColor = _color;
            System.Console.WriteLine(text);
            System.Console.ResetColor();
        }
    }
}
