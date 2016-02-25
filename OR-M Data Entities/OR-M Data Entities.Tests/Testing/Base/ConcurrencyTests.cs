using System;
using System.Data;
using OR_M_Data_Entities.Tests.Tables;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public class ConcurrencyTests
    {
        private static bool _was_Test_C_3_handlerFired { get; set; }

        private static void _handle(object entity)
        {
            _was_Test_C_3_handlerFired = true;
        }

        public static bool Test_1()
        {
            // concurrency test - Exception
            try
            {
                // reset the name
                var concurrencyExceptionContext = new ConcurrencyExceptionContext();
                var ctx = new DefaultContext();
                var id = 3;

                var name = concurrencyExceptionContext.Find<Name>(id);
                name.Value = "New Name";
                concurrencyExceptionContext.SaveChanges(name);

                // start the test

                // perform update before concurrencyExceptionContext
                var n = ctx.Find<Name>(id);
                n.Value = "Megan";
                ctx.SaveChanges(n);

                // try to update
                name.Value = "Other Name";
                concurrencyExceptionContext.SaveChanges(name);

                return false;
            }
            catch (Exception ex)
            {
                return ex is DBConcurrencyException;
            }
        }

        public static bool Test_2()
        {
            // concurrency test - Continue
            try
            {
                // reset the name
                var concurrencyContinueContext = new ConcurrencyContinueContext();
                var ctx = new DefaultContext();
                var id = 3;

                var name = concurrencyContinueContext.Find<Name>(id);
                name.Value = "New Name";
                concurrencyContinueContext.SaveChanges(name);

                // start the test

                // perform update before concurrencyExceptionContext
                var n = ctx.Find<Name>(id);
                n.Value = "Megan";
                ctx.SaveChanges(n);

                // try to update
                name.Value = "Other Name";
                concurrencyContinueContext.SaveChanges(name);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Test_3()
        {
            // concurrency test - Use Handler
            try
            {
                // reset the name
                var concurrencyHandleContext = new ConcurrencyHandleContext();
                var ctx = new DefaultContext();
                var id = 3;

                var name = concurrencyHandleContext.Find<Name>(id);
                name.Value = "New Name";
                concurrencyHandleContext.SaveChanges(name);

                // start the test

                // perform update before concurrencyExceptionContext
                var n = ctx.Find<Name>(id);
                n.Value = "Megan";
                ctx.SaveChanges(n);

                // try to update
                name.Value = "Other Name";
                concurrencyHandleContext.SaveChanges(name);

                return false;
            }
            catch (Exception ex)
            {
                return ex is NotImplementedException;
            }
        }
    }
}
