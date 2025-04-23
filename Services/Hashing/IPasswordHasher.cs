namespace Services.Hashing
{
    public interface  IPasswordHasher
    {
       public void  CreateHash(string password, out byte[] hash, out byte[] salt);
        public bool Verify(string password, byte[] hash, byte[] salt); 
    }
}
