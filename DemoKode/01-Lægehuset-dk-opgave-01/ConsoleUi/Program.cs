// See https://aka.ms/new-console-template for more information

using Domain.Entity;

Console.WriteLine("Hello, World!");

var læge = new Læge();
var patient = new Patient();
var konsultationstype = new Vaccination();
var startTid = DateTime.Now.AddDays(1);
var aftale = new Konsultationsaftale(konsultationstype, læge, patient, startTid);

var nyKonsultaionstype = new AlmindeligKonsultation();
aftale.UdskiftKonsultationsTypen(nyKonsultaionstype);
Console.WriteLine();
