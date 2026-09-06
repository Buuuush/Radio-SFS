// ---------- DESCRIPTION --------- \\
/*                                  *\

1. Temporarily store audio data
2. Allow the network to write while Unity reads
3. Avoid audio outages

Help : circular buffer
https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1
https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock

Useful links :
https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-basics
https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1

*\                                   */
// -------- END DESCRIPTION -------- \\