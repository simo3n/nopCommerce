using System;

namespace Nop.Services.Media.RoxyFileman
{
    public partial record RoxyImageInfo(string Name, DateTimeOffset LastWriteTime, long FileLength, int Width, int Height);
}