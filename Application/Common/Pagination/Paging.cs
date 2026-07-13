namespace Application.Common.Pagination
{
    public static class Paging
    {
        public const int DefaultTake = 20;
        public const int MaxTake = 100;

        public static (int Skip, int Take) Normalize(int skip, int take)
        {
            if (skip < 0)
                skip = 0;

            if (take <= 0)
                take = DefaultTake;

            if (take > MaxTake)
                take = MaxTake;

            return (skip, take);
        }
    }
}
