using Shotgun.Controllers;
using TEST.Shotgun.API.Domain;
using TEST.Shotgun.API.Infrastructure;

namespace TEST.Shotgun.API.Controllers;

// The "given controller that inherits the Shotgun class" that the E2E harness
// drives over real HTTP. No overrides needed — every endpoint comes from the base.
public class TestEntityController : Shotgun<TestEntity, TestRepository, int>
{
    public TestEntityController(TestRepository repository) : base(repository) { }
}
