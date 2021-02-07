namespace Logging.Layout
{
    public static class LogLayout
    {
        public static string JsonLogLayout =
            "{ \"timestamp\": \"${longdate}\", \"level\" : \"${level}\", \"machine_name\" : \"${machinename}\" ," +
            " \"process_id\" : ${gdc:item=ProcessId} , \"thread_id\" : \"${threadid:padding=5:padCharacter=0}\", " +
            "\"source\" : \"${gdc:item=Source}\", \"sequence_id\" : ${sequence_id}, \"class\" : \"${logger}\", ${json_data} }";
        
        public static string DefaultLayout = "${longdate} @${threadid:padding=5:padCharacter=0} ${message} ${exception}";
    }
}

