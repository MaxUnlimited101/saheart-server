namespace saheart_server
{
    public static class IListExtender
    {
        // https://stackoverflow.com/questions/273313/randomize-a-listt
        public static Random rng = new Random(0);
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // https://stackoverflow.com/questions/222598/how-do-i-clone-a-generic-list-in-c
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}
