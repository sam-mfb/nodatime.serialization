// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using Newtonsoft.Json;
using NodaTime.Text;

namespace NodaTime.Serialization.JsonNet
{
    /// <summary>
    /// Convenience class to expose preconfigured converters for Noda Time types, and factory methods
    /// for creating those which require parameters.
    /// </summary>
    public static class NodaConverters
    {
        /// <summary>
        /// Converter for instants, using the ISO-8601 date/time pattern, extended as required to accommodate nanoseconds, and
        /// specifying 'Z' at the end to show it's effectively in UTC.
        /// </summary>
        public static JsonConverter InstantConverter { get; }
            = new NodaPatternConverter<Instant>(InstantPattern.ExtendedIso);

        /// <summary>
        /// Converter for local dates, using the ISO-8601 date pattern.
        /// </summary>
        public static JsonConverter LocalDateConverter { get; }
            = new NodaPatternConverter<LocalDate>(
                LocalDatePattern.Iso, CreateIsoValidator<LocalDate>(x => x.Calendar));

        /// <summary>
        /// Converter for local dates and times, using the ISO-8601 date/time pattern, extended as required to accommodate nanoseconds.
        /// No time zone designator is applied.
        /// </summary>
        public static JsonConverter LocalDateTimeConverter { get; }
            = new NodaPatternConverter<LocalDateTime>(
                LocalDateTimePattern.ExtendedIso, CreateIsoValidator<LocalDateTime>(x => x.Calendar));

        /// <summary>
        /// Converter for local times, using the ISO-8601 time pattern, extended as required to accommodate nanoseconds.
        /// </summary>
        public static JsonConverter LocalTimeConverter { get; }
            = new NodaPatternConverter<LocalTime>(LocalTimePattern.ExtendedIso);

        /// <summary>
        /// Converter for annual dates, using an ISO-8601 compatible pattern for the month and day parts.
        /// </summary>
        public static JsonConverter AnnualDateConverter { get; }
            = new NodaPatternConverter<AnnualDate>(AnnualDatePattern.Iso);

        /// <summary>
        /// Converter for intervals. This must be used in a serializer which also has an instant converter.
        /// </summary>
        public static JsonConverter IntervalConverter { get; } = new NodaIntervalConverter();

        /// <summary>
        /// Converter for intervals using extended ISO-8601 format, as output by <see cref="Interval.ToString"/>.
        /// </summary>
        public static JsonConverter IsoIntervalConverter { get; }
            = new NodaIsoIntervalConverter();

        /// <summary>
        /// Converter for date intervals. This must be used in a serializer which also has a local date converter.
        /// </summary>
        public static JsonConverter DateIntervalConverter { get; } = new NodaDateIntervalConverter();

        /// <summary>
        /// Converter for date intervals using ISO-8601 format, as defined by <see cref="LocalDatePattern.Iso"/>.
        /// </summary>
        public static JsonConverter IsoDateIntervalConverter { get; }
            = new NodaIsoDateIntervalConverter();

        /// <summary>
        /// Converter for offsets.
        /// </summary>
        public static JsonConverter OffsetConverter { get; }
            = new NodaPatternConverter<Offset>(OffsetPattern.GeneralInvariant);

        /// <summary>
        /// Converter for offset date/times.
        /// </summary>
        public static JsonConverter OffsetDateTimeConverter { get; } =
            new NodaPatternConverter<OffsetDateTime>(
                OffsetDateTimePattern.Rfc3339, CreateIsoValidator<OffsetDateTime>(x => x.Calendar));

        /// <summary>
        /// Converter for offset dates.
        /// </summary>
        public static JsonConverter OffsetDateConverter { get; } =
            new NodaPatternConverter<OffsetDate>(
                OffsetDatePattern.GeneralIso, CreateIsoValidator<OffsetDate>(x => x.Calendar));

        /// <summary>
        /// Converter for offset times.
        /// </summary>
        public static JsonConverter OffsetTimeConverter { get; } =
            new NodaPatternConverter<OffsetTime>(OffsetTimePattern.ExtendedIso);

        /// <summary>
        /// Creates a converter for zoned date/times, using the given time zone provider.
        /// </summary>
        /// <param name="provider">The time zone provider to use when parsing.</param>
        /// <returns>A converter to handle <see cref="ZonedDateTime"/>.</returns>
        public static JsonConverter CreateZonedDateTimeConverter(IDateTimeZoneProvider provider) =>
            new NodaPatternConverter<ZonedDateTime>(
                ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFFo<G> z", provider),
                CreateIsoValidator<ZonedDateTime>(x => x.Calendar));

        /// <summary>
        /// Create a converter for zoned date/times that uses the format defined in the
        /// [proposed] extension to RFC 3339 for indicating time zones in IANA format
        /// </summary>
        /// <returns></returns>
        public static JsonConverter CreateZonedDateTimeRFC3339() =>
            new NodaPatternConverter<ZonedDateTime>(
                ZonedDateTimePattern.CreateWithInvariantCulture("uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFo<G>'['z']'", DateTimeZoneProviders.Tzdb),
                CreateIsoValidator<ZonedDateTime>(x => x.Calendar));

        /// <summary>
        /// Creates a converter for time zones, using the given provider.
        /// </summary>
        /// <param name="provider">The time zone provider to use when parsing.</param>
        /// <returns>A converter to handle <see cref="DateTimeZone"/>.</returns>
        public static JsonConverter CreateDateTimeZoneConverter(IDateTimeZoneProvider provider) =>
            new NodaDateTimeZoneConverter(provider);

        /// <summary>
        /// Converter for durations.
        /// </summary>
        public static JsonConverter DurationConverter { get; }
            = new NodaPatternConverter<Duration>(DurationPattern.CreateWithInvariantCulture("-H:mm:ss.FFFFFFFFF"));

        /// <summary>
        /// Round-tripping converter for periods. Use this when you really want to preserve information,
        /// and don't need interoperability with systems expecting ISO.
        /// </summary>
        public static JsonConverter RoundtripPeriodConverter { get; }
            = new NodaPatternConverter<Period>(PeriodPattern.Roundtrip);

        /// <summary>
        /// Normalizing ISO converter for periods. Use this when you want compatibility with systems expecting
        /// ISO durations (~= Noda Time periods). However, note that Noda Time can have negative periods. Note that
        /// this converter loses information - after serialization and deserialization, "90 minutes" will become "an hour and 30 minutes".
        /// </summary>
        public static JsonConverter NormalizingIsoPeriodConverter { get; }
            = new NodaPatternConverter<Period>(PeriodPattern.NormalizingIso);

        private static Action<T> CreateIsoValidator<T>(Func<T, CalendarSystem> calendarProjection) => value =>
        {
            var calendar = calendarProjection(value);
            // We rely on CalendarSystem.Iso being a singleton here.
            Preconditions.CheckArgument(calendar == CalendarSystem.Iso,
                "Values of type {0} must (currently) use the ISO calendar in order to be serialized.",
                typeof(T).Name);
        };
    }
}
