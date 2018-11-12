public interface IMessenger
{
    int Register();
    void SendMessage(int id, byte[] message);
    byte[] GetMessage();
    void ClearMessage();
}
