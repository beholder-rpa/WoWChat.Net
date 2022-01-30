namespace WoWChat.Net.Game
{
  /// <summary>
  /// Used for Warden and WotLK Header Encryption
  /// </summary>
  public class RC4
  {
    private const int STATE_LENGTH = 256;
    private readonly byte[] _state;
    private int _i = 0, _j = 0;

    public RC4(byte[] key)
    {
      _state = SetupKey(key);
    }

    public byte[] Crypt(byte[] msg)
    {
      var code = new byte[msg.Length];
      for (int n = 0; n < msg.Length; n++)
      {
        _i = (_i + 1) % STATE_LENGTH;
        _j = (_j + _state[_i]) % STATE_LENGTH;
        Swap(_state, _i, _j);
        var rand = _state[(_state[_i] + _state[_j]) % STATE_LENGTH];
        code[n] = (byte)(rand ^ msg[n]);
      }
      return code;
    }

    private static byte[] SetupKey(byte[] key)
    {
      var state = new byte[STATE_LENGTH];

      for (int i = 0; i < STATE_LENGTH; i++)
      {
        state[i] = (byte)i;
      }

      var j = 0;
      for (int i = 0; i < STATE_LENGTH; i++)
      {
        j = (j + state[i] + key[i % key.Length] + STATE_LENGTH) % STATE_LENGTH;
        Swap(state, i, j);
      }
      return state;
    }

    private static void Swap(byte[] state, int i, int j)
    {
      var tmp = state[i];
      state[i] = state[j];
      state[j] = tmp;
    }
  }
}
