using System.Text;
using System.Text.RegularExpressions;

bool debug = true;
bool dryrun = false;

string argNumberOfCharacters = null;
string argPathToScripts = null;
string argListOfScripts = null;
string argPathToMessage = null;

if (args.Length == 0) {
    InvalidArguments();
}

for (int i = 0; i < args.Length; i++) {
    switch (args[i]) {
        case "-c":
            if (argNumberOfCharacters != null) {
                InvalidArguments();
            }
            argNumberOfCharacters = args[i + 1];
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
        case "-m":
            if (argPathToMessage != null) {
                InvalidArguments();
            }
            argPathToMessage = args[i + 1].Replace(@"'", "");
            break;
        case "--dry-run":
            dryrun = true;
            break;
    }
}

if (argNumberOfCharacters == null || argPathToScripts == null || argListOfScripts == null || argPathToMessage == null) {
    InvalidArguments();
}

if (debug) {
    Console.WriteLine("-c is: " + argNumberOfCharacters);
    Console.WriteLine("-p is: " + argPathToScripts);
    Console.WriteLine("-s is: " + argListOfScripts);
}

#region text formatting
if (argNumberOfCharacters != null && argPathToScripts != null && argListOfScripts != null) {
    // delete message file at start
    File.Delete(argPathToMessage);

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
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Formatting file: " + fullScriptPath);
            Console.ResetColor();

            string[] textInFile = File.ReadAllLines(fullScriptPath, Encoding.UTF8);

            int c;
            int n;
            // test if -n and -c are indeed convertable to int
            try {
                c = Int16.Parse(argNumberOfCharacters);
            }
            catch {
                InvalidArguments();
            }
            c = Int16.Parse(argNumberOfCharacters);

            for (int i = 0; i < textInFile.Length; i++) {
                Match dialogueLine = Regex.Match(textInFile[i], @"(\◇|\◆).{9}(\◇|\◆)(.+)");
                Match dialogueLineAddress = Regex.Match(textInFile[i], @"((\◇|\◆).{9}(\◇|\◆)).+");
                if (dialogueLine.Success) {
                    if (dialogueLine.Groups[3].Value.Length > c) {
                        textInFile[i] = FormatText(dialogueLineAddress.Groups[1].Value, dialogueLine.Groups[3].Value, c, debug);
                    }
                }
            }
            // commit changes by writting the lines to file
            if (!dryrun)
                File.AppendAllLines(argPathToMessage, textInFile);
        }
        else {
            FileNotFound();
        }
    }
}
#endregion

string FormatText(string dialogueLineId, string input, int charsPerLine, bool debug) {
    int currentIndexPosition = 0;
    while (input.Length > currentIndexPosition + charsPerLine) {
        currentIndexPosition += charsPerLine;
        while (input[currentIndexPosition] != ' ') {
            // 『』kagikakko handling
            if (input[currentIndexPosition] == '『') {
                //currentIndexPosition--;
                break;
            }
            //else if (input[currentIndexPosition] == '』') {
            //    currentIndexPosition++;
            //    break;
            //}
            else {
                currentIndexPosition--;
            }
        }
        input = input.Insert(currentIndexPosition, "\\n");
        currentIndexPosition = input.LastIndexOf("\\n") + 2;
        // removes white space at the beginning of new line
        if (input[currentIndexPosition] == ' ') {
            input = input.Remove(currentIndexPosition, 1);
        }
    }

    if (debug) {
        Console.WriteLine(dialogueLineId + input);
    }

    return dialogueLineId + input;
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
    Console.WriteLine("Atelier Kaguya visual novel game engine text formatter\n" +
        "This tool formats the characters of translated script files by replacing characters\n" +
        "of incomplete words that are at the right edge of the dialogue lines with \\n new line\n" +
        "character in order to better format the text\n" +
        "\n" +
        "To use this tool:\n" +
        "-c : number of characters per line\n" +
        "-p : path to Script files between single quotes\n" +
        "-s : comma separated list of script files that need formatting\n" +
        "-m : path to message.txt file for conversion to message.dat\n" +
        "--dry-run : [optional] will only output the changes and not modify the files" +
        "\n" +
        "Example usage: KaguyaTextFormatter.exe -c 50 -n 3 -p 'C:\\path\\to\\scripts' -s sc_a01,sc_a02,sc_a03" +
        "\n");
}