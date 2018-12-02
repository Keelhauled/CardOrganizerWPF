public interface IMessenger
{
    void SendMessage(string id, byte[] message);
    byte[] GetMessage(string id);
    void ClearMessage(string id);
}
