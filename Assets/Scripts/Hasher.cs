using System;
using System.Security.Cryptography;
using System.Text;

public static class Hasher
{
    public static string GetHash(HashAlgorithm _hashAlgorithm, byte[] _buffer)
    {
        byte[] _data = _hashAlgorithm.ComputeHash(_buffer);
        
        StringBuilder _sBuilder = new StringBuilder();
        for (int _i = 0; _i < _data.Length; _i++)
        {
            _sBuilder.Append(_data[_i].ToString("x2"));
        }

        return _sBuilder.ToString();
    }

    public static bool Compare(HashAlgorithm _hashAlgorithm, string _hash1, string _hash2)
    {
        StringComparer _comparer = StringComparer.OrdinalIgnoreCase;
        return _comparer.Compare(_hash1, _hash2) == 0;
    }
}