using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3248
{
	public class ByCodeFixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
                rc.Property(x => x.DeletionDate);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var e1 = new Entity { Name = "Bob", DeletionDate = null };
				session.Save(e1);

				var e2 = new Entity { Name = "Sally", DeletionDate = new DateTime(2012, 01, 18)};
				session.Save(e2);

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

        // See: https://nhibernate.jira.com/browse/NH-3248
		[Test]
		public void GroupByNullableDatePart_MustNotThrowAnException()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
                var result = (from e in session.Query<Entity>()
                              group e by e.DeletionDate != null ? e.DeletionDate.Value.Date : e.DeletionDate into gr
                              select new { Key = gr.Key, Count = gr.Count() })
                             .ToList();

				Assert.AreEqual(2, result.ToList().Count);
			}
		}
	}
}