namespace Mine
{
    public class Random
    {
        int x = 97;
        public bool createRandom()
        {
            int a = UnityEngine.Random.Range(1000, 10000);
            int r = UnityEngine.Random.Range(1, a);
            

            return r % x == 0;
        }
    }
}