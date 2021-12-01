using AutoFixture;
using AutoFixture.Kernel;
using Domain.Models;
using System;
using System.Reflection;

namespace TvShowApi.Tests
{
    public class DateOfBirthDateTimeBuilder : ISpecimenBuilder
    {
        private readonly RandomDateTimeSequenceGenerator _dateOfBirthGenerator;

        public DateOfBirthDateTimeBuilder()
        {
            var oneHundredYearsOld = DateTime.Today.AddYears(-100);
            var eighteenYearsOld = DateTime.Today.AddYears(-18);
            _dateOfBirthGenerator = new RandomDateTimeSequenceGenerator(oneHundredYearsOld, eighteenYearsOld);
        }

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as PropertyInfo;

            if (pi?.PropertyType == typeof(DateTime?)
                && pi.Name == nameof(Cast.Birthday))
            {

                var dateTime = (DateTime)_dateOfBirthGenerator.Create(typeof(DateTime), context);
                return dateTime.Date;
            }

            return new NoSpecimen();
        }
    }
}
