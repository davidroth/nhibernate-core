using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3373
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
                rc.Component(x => x.Address);
			});
            mapper.Class<Country>(rc =>
            {
                rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
                rc.Property(x => x.Name);
            });
            mapper.Component<AddressComponent>(rc =>
                {
                    rc.Property(x => x.Street);
                    rc.ManyToOne(y => y.Country);
                });

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
                var c1 = new Country { Name = "Austria" };
                session.Save(c1);

				var e1 = new Company { Name = "Bob" };
                e1.Address = new AddressComponent();
                e1.Address.Country = c1;
				session.Save(e1);

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

        // Country Entity on component not eager laoded, see bug: https://nhibernate.jira.com/browse/NH-3373
		[Test]
		public void LinqFetchThroughComponent_EntityOnComponentEagerLoaded()
		{
            Company company = null;

			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
                company = session.Query<Company>()
                    .Fetch(x => x.Address).ThenFetch(x => x.Country)
                    .SingleOrDefault();
			}

            Assert.IsTrue(NHibernateUtil.IsInitialized(company.Address.Country), "Country entity on Component 'AddressComponent' not loaded!");
		}
	}
}