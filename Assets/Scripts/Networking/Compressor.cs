using System;
using System.IO;
using System.IO.Compression;

public static class Compressor
{
    public static byte[] CompressBytes(byte[] _buffer)
    {
        MemoryStream _ms = new MemoryStream();
        GZipStream _zip = new GZipStream(_ms, CompressionMode.Compress, true);
        _zip.Write(_buffer, 0, _buffer.Length);
        _zip.Close();
        _ms.Position = 0;
        
        MemoryStream _outStream = new MemoryStream();

        byte[] _compressed = new byte[_ms.Length];
        _ms.Read(_compressed, 0, _compressed.Length);

        byte[] _gzBuffer = new byte[_compressed.Length + 4];
        Buffer.BlockCopy(_compressed, 0, _gzBuffer, 4, _compressed.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(_buffer.Length), 0, _gzBuffer, 0, 4);
        return _gzBuffer;
    }

    public static byte[] DecompressBytes(byte[] _gzBuffer)
    {
        MemoryStream _ms = new MemoryStream();
        int _msgLength = BitConverter.ToInt32(_gzBuffer, 0);
        _ms.Write(_gzBuffer, 4, _gzBuffer.Length - 4);

        byte[] _buffer = new byte[_msgLength];

        _ms.Position = 0;
        GZipStream _zip = new GZipStream(_ms, CompressionMode.Decompress);
        _zip.Read(_buffer, 0, _buffer.Length);

        return _buffer;
    }
}