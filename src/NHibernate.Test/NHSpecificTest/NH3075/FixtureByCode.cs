using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3075
{
	/// <summary>
	/// Fixture using 'by code' mappings
	/// </summary>
	/// <remarks>
	/// This fixture is identical to <see cref="Fixture" /> except the <see cref="Company" /> mapping is performed 
	/// by code in the GetMappings method, and does not require the <c>Mappings.hbm.xml</c> file. Use this approach
	/// if you prefer.
	/// </remarks>
	public class ByCodeFixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Company>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
                rc.ManyToOne(x => x.Person);
                rc.Table("tbl_Company");
			});
            mapper.Class<Person>(rc =>
            {
                rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
                rc.Property(x => x.Name);
                rc.Table("tbl_User");
            });

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
                var p1 = new Person { Name = "User1" };
                var p2 = new Person { Name = "User2" };
                session.Save(p1);
                session.Save(p2);

				var e1 = new Company { Name = "Bob" };
                e1.Person = p1;
				session.Save(e1);

				var e2 = new Company { Name = "Sally" };
                e2.Person = p2;
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

        // Throws a null reference exception, see bug: https://nhibernate.jira.com/browse/NH-3075
		[Test]
		public void GroupByWithFetch_DoesNotThrowNullReferenceException()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
                var result = session.Query<Company>()
                    .Where(x => x.Name == "Sally")
                    .Fetch(x => x.Person)
                    .GroupBy(x => x.Name)
                    .Select(x => x.Key)
                    .ToList();

				Assert.AreEqual(1, result.ToList().Count);
			}
		}
	}
}