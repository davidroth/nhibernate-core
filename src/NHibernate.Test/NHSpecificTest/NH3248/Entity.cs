using System;

namespace NHibernate.Test.NHSpecificTest.NH3248
{
    class Entity
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime? DeletionDate { get; set; }
    }
}