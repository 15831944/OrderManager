namespace OrderManagerNew.V2Implant
{
    class ToothType
    {
        public enum ToothTypeList
        {
            NON_TOOTH = 0,
            OFFSET_COPING,
            ANATOMIC_COPING,
            ANATOMIC_CROWN,
            OFFSET_PONTIC,
            REDUCED_PONTIC,
            ANATOMIC_PONTIC,
            VENEER,
            INLAYONLAY,
            ABUTMENT,
            ABUTMENT_OFFSET_COPING,
            ABUTMENT_ANATOMIC_COPING,
            ABUTMENT_ANATOMIC_CROWN,
            ABUTMENT_TEMP,
            SRIA,
            SRIA_CROWN,
            NEIGHBOR,
            IMPLANT
        }

        public enum ProjectIconType : int
        {
            DesignerIcon = 1,
            ImplantIcon = 2,
            BarIcon = 3,
            CADIcon = 4,
            TrayIcon = 5,
            SplintIcon=6,
            OrthoIcon =7,

            DesignerDownloadIcon = 11,
            ImplantDownloadIcon = 12,

            UploadIcon = 100,
            DownloadIcon = 101
        }
    }
}