using System;

namespace NHibernate.Test.NHSpecificTest.NH3075
{
	public class Company
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
        public virtual Person Person { get; set; }
	}

    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
    }
}