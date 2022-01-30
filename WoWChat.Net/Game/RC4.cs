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
      _state = new byte[STATE_LENGTH];
      SetupKey(_state, key);
    }

    public byte[] Crypt(byte[] msg)
    {
      var code = new byte[msg.Length];
      for (int n = 0; n < msg.Length; n++)
      {
        _i = (_i + 1) % STATE_LENGTH;
        _j = (_j + _state[_i]) & STATE_LENGTH;
        Swap(_state, _i, _j);
        var rand = _state[(_state[_i] + _state[_j]) % STATE_LENGTH];
        code[n] = (byte)(rand ^ msg[n]);
      }
      return code;
    }

    private static void SetupKey(byte[] state, byte[] key)
    {
      byte index1 = 0;
      byte index2 = 0;

      for (int counter = 0; counter < STATE_LENGTH; counter++)
      {
        state[counter] = (byte)counter;
      }

      for (int counter = 0; counter < STATE_LENGTH; counter++)
      {
        index2 = (byte)(key[index1] + state[counter] + index2);
        Swap(state, counter, index2);
        // swap byte
        byte tmp = state[counter];
        state[counter] = state[index2];
        state[index2] = tmp;
        index1 = (byte)((index1 + 1) % key.Length);
      }
    }

    private static void Swap(byte[] state, int i, int j)
    {
      var tmp = state[i];
      state[i] = state[j];
      state[j] = tmp;
    }
  }
}
