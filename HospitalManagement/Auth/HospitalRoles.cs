namespace HospitalManagement.Auth
{
    public static class HospitalRoles
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Nurse = "Nurse";
        public const string Receptionist = "Receptionist";
        public const string Pharmacist = "Pharmacist";
        public const string LabTechnician = "LabTechnician";
        public const string Patient = "Patient";

        public const string AdminOrDoctor = "Admin,Doctor";
        public const string AdminOrReceptionist = "Admin,Receptionist";
        public const string ClinicalStaff = "Admin,Doctor,Nurse";
        public const string MedicalTeam = "Admin,Doctor,Nurse,LabTechnician";
        public const string AllStaff = "Admin,Doctor,Nurse,Receptionist,Pharmacist,LabTechnician";
    }
}