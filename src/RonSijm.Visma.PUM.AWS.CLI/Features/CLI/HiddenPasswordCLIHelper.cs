using System.Text;

namespace RonSijm.VismaPUM.CLI.Features.CLI;

public static class HiddenPasswordCLIHelper
{
    public static string HidePassword()
    {
        var password = new StringBuilder();
        do
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                Console.Write("\b \b");
                password.Length--;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                Console.Write("*");
                password.Append(key.KeyChar);
            }
        } while (true);

        return password.ToString();
    }
}