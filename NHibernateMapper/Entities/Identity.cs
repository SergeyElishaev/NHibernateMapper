namespace NHibernateAttributesMapper.Entities
{
    public class Identity
    {
        public Identity()
        {
            IsIdentity = false;
            Seed = 1;
            Increment = 1;
        }

        public bool IsIdentity { get; set; }
        public int Seed { get; set; }
        public int Increment { get; set; }
    }
}
