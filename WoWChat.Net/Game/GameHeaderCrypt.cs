namespace WoWChat.Net.Game
{
  using System;

  public class GameHeaderCrypt
  {
    private int _send_i = 0;
    private int _send_j = 0;
    private int _recv_i = 0;
    private int _recv_j = 0;
    protected byte[] _key = Array.Empty<byte>();

    public virtual bool IsInitialized { get; protected set; }

    public virtual byte[] Decrypt(byte[] data)
    {
      if (!IsInitialized)
      {
        return data;
      }

      foreach (var i in data)
      {
        _recv_i %= _key.Length;
        var x = (byte)((data[i] - _recv_j) ^ _key[_recv_i]);
        _recv_i += 1;
        _recv_j = data[i];
        data[i] = x;
      }

      return data;
    }

    public virtual byte[] Encrypt(byte[] data)
    {
      if (!IsInitialized)
      {
        return data;
      }

      foreach (var i in data)
      {
        _send_i %= _key.Length;
        var x = (byte)((data[i] ^ _key[_send_i]) + _send_j);
        _send_i += 1;
        data[i] = x;
        _send_j = x;
      }

      return data;
    }

    public virtual void Init(byte[] key)
    {
      _key = key;
      _send_i = 0;
      _send_j = 0;
      _recv_i = 0;
      _recv_j = 0;
      IsInitialized = true;
    }
  }
}