public interface IMessenger
{
    void SendMessage(string process, byte[] message);
    byte[] GetMessage(string process);
    void ClearMessage(string process);
}
