namespace komikaan.Context
{
    public class DatabaseAgency
    {
        public string DataOrigin { get; set; } // data_origin
        public string Id { get; set; }         // id
        public string? Name { get; set; }      // name
        public string? Url { get; set; }       // url
        public string? Timezone { get; set; }  // timezone
        public string? LanguageCode { get; set; } // language_code
        public string? Phone { get; set; }     // phone
        public string? FareUrl { get; set; }   // fare_url
        public string? Email { get; set; }     // email
        public Guid InternalId { get; set; }   // internal_id
        public DateTimeOffset LastUpdated { get; set; } // last_updated
        public Guid ImportId { get; set; }  // import_id
    }
}
