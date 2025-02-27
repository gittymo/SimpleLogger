// See https://aka.ms/new-console-template for more information
using SimpleLogger;

Logger logger = new Logger("log.txt", 5);
logger.Log("INFO", "Hello, {0}!", "World");

Console.WriteLine("Hello, World!");
