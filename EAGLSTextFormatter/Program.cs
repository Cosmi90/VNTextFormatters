using System.Text.RegularExpressions;

bool debug = true;

string argNumberOfCharacters = null;
string argNumberOfLines = null;
string argPathToScripts = null;
string argListOfScripts = null;

if (args.Length == 0) {
    InvalidArguments();
}

for (int i = 0; i < args.Length; i++) {
    switch(args[i]) {
        case "-c":
            if (argNumberOfCharacters != null) {
                InvalidArguments();
            }
            argNumberOfCharacters = args[i + 1];
            break;
        case "-n":
            if (argNumberOfLines != null) {
                InvalidArguments();
            }
            argNumberOfLines = args[i + 1];
            break;
        case "-p":
            if (argPathToScripts != null) {
                InvalidArguments();
            }
            argPathToScripts = args[i + 1].Replace(@"'", "");
            break;
        case "-s":
            if (argListOfScripts != null) {
                InvalidArguments();
            }
            argListOfScripts = args[i + 1];
            break;
    }
}

if (argNumberOfCharacters == null || argNumberOfLines == null || argPathToScripts == null || argListOfScripts == null) {
    InvalidArguments();
}

if (debug) {
    Console.WriteLine("-c is: " + argNumberOfCharacters);
    Console.WriteLine("-n is: " + argNumberOfLines);
    Console.WriteLine("-p is: " + argPathToScripts);
    Console.WriteLine("-s is: " + argListOfScripts);
}

#region text formatting
if (argNumberOfCharacters != null && argNumberOfLines != null && argPathToScripts != null && argListOfScripts != null) {
    string[] scriptFiles = argListOfScripts.Split(',');

    foreach (string s in scriptFiles) {
        string fullScriptPath;
        if (!File.Exists(argPathToScripts + "\\" + s)) {
            fullScriptPath = argPathToScripts + "\\" + s + ".txt";
        }
        else {
            fullScriptPath = argPathToScripts + "\\" + s;
        }

        if (File.Exists(fullScriptPath)) {
            Console.WriteLine("Formatting file: " + fullScriptPath);

            string[] textInFile = File.ReadAllLines(fullScriptPath);

            foreach (string t in textInFile) {
                Match dialogueLine = Regex.Match(t, @"\&[0-9]{1,5}""(.+)""");
                if (dialogueLine.Success) {
                    if (debug) {
                        Console.WriteLine(dialogueLine.Groups[1].Value);
                    }

                    // test if -n and -c are indeed convertable to int
                    try {
                        int c = Int16.Parse(argNumberOfCharacters);
                        int n = Int16.Parse(argNumberOfLines);
                    }
                    catch {
                        InvalidArguments();
                    }

                    Console.WriteLine(FormatText(dialogueLine.Groups[1].Value, Int16.Parse(argNumberOfCharacters), Int16.Parse(argNumberOfLines)));
                }
            }
        }
        else {
            FileNotFound();
        }
    }
}
#endregion

string FormatText(string input, int charsPerLine, int nrOfLines) {
    if (input.Length > charsPerLine) {
        if (input[charsPerLine] == ' ') {
            Console.WriteLine("LINE has empty on: " + charsPerLine + " character: " + input);
        }
    } 

    return "placeholder";
}

void FileNotFound() {
    Console.WriteLine("Specified path does not exist!\n");
    Environment.Exit(1);
}

void InvalidArguments() {
    Console.WriteLine("Missing or invalid arguments specified!\n");
    PrintHelp();
    Environment.Exit(1);
}

void PrintHelp() {
    Console.WriteLine("EAGLS visual novel game engine text formatter\n" +
        "This tool formats the characters of translated script files by replacing characters\n" +
        "of incomplete words that are the the right edge of the dialogue lines with (e) new line\n" +
        "character in order to better format the text, in case of reaching the dialogue line\n" +
        "limit, it will try to create a new dialogue line with the remainder of the text\n" +
        "\n" +
        "To use this tool:\n" +
        "-c : number of characters per line\n" +
        "-n : number of lines per dialogue box\n" +
        "-p : path to Script files between single quotes\n" +
        "-s : comma separated list of script files that need formatting\n" +
        "\n" +
        "Example usage: EAGLSTextFormatter.exe -c 50 -n 3 -p 'C:\\path\\to\\scripts' -s sc_a01,sc_a02,sc_a03" +
        "\n");
}