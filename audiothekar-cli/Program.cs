// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using audiothek_client;

Console.WriteLine("Starting...");
ApiRequester apiRequester = new ApiRequester();
IEnumerable<string> titles = apiRequester.GetAllProgramSets().Result;
foreach (string title in titles)
{
    Console.WriteLine(title);
}
Console.WriteLine("Which one to download?");
string? inReadLine = Console.ReadLine();
if (inReadLine == null)
    return;
string path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
Task downloadTask = apiRequester.DownloadAllFilesByTitle(inReadLine, path);
Console.WriteLine("Downloading...");
await downloadTask;
Console.WriteLine("Finished.");