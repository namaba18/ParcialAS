using System;
using System.Collections.Generic;

public class MementoCandidato
{
    public IEstadoCandidato EstadoGuardado { get; private set; }

    public MementoCandidato(IEstadoCandidato estado)
    {
        EstadoGuardado = estado;
    }
}

public interface IObservador
{
    void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo);
}

public class ObservadorReclutador : IObservador
{
    public void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo)
    {        
        Console.WriteLine($"[Notificación Reclutador]: {candidato.Nombre} pasó de {estadoAnterior} a {estadoNuevo}.");
    }
}

public class ObservadorGerente : IObservador
{
    public void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo)
    {
        if (estadoNuevo == "OFERTA" || estadoNuevo == "CONTRATADO")
        {
            Console.WriteLine($"[Notificación Gerente]: Atención, {candidato.Nombre} avanzó a {estadoNuevo}.");
        }
    }
}

public class ObservadorNomina : IObservador
{
    public void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo)
    {
        if (estadoNuevo == "CONTRATADO")
        {
            Console.WriteLine($"[Notificación Nómina]: {candidato.Nombre} fue contratado. Iniciar proceso de afiliación.");
        }
    }
}

public class ObservadorPortal : IObservador
{
    public void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo)
    {
        Console.WriteLine($"[Actualización Portal]: Tu estado en el proceso ha cambiado a {estadoNuevo}.");
    }
}

public interface IEstadoCandidato
{
    string Nombre { get; }
    void Avanzar(Candidato candidato, string nuevoEstado);
}

public class EstadoAplicado : IEstadoCandidato
{
    public string Nombre => "APLICADO";
    
    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "ENTREVISTA")
            candidato.SetEstado(new EstadoEntrevista());
        else if (nuevoEstado == "RECHAZADO")
            candidato.SetEstado(new EstadoRechazado());
        else
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
    }
}

public class EstadoEntrevista : IEstadoCandidato
{
    public string Nombre => "ENTREVISTA";
    
    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "PRUEBA TECNICA")
            candidato.SetEstado(new EstadoPruebaTecnica());
        else if (nuevoEstado == "RECHAZADO")
            candidato.SetEstado(new EstadoRechazado());
        else
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
    }
}

public class EstadoPruebaTecnica : IEstadoCandidato
{
    public string Nombre => "PRUEBA TECNICA";
    
    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "OFERTA")
            candidato.SetEstado(new EstadoOferta());
        else if (nuevoEstado == "RECHAZADO")
            candidato.SetEstado(new EstadoRechazado());
        else
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
    }
}

public class EstadoOferta : IEstadoCandidato
{
    public string Nombre => "OFERTA";

    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "VERIFICACION DE REFERENCIAS")
            candidato.SetEstado(new EstadoVerificacionReferencias());
        else if (nuevoEstado == "RECHAZADO")
            candidato.SetEstado(new EstadoRechazado());
        else
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
    }
}

public class EstadoVerificacionReferencias : IEstadoCandidato
{
    public string Nombre => "VERIFICACION DE REFERENCIAS";

    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "CONTRATADO")
            candidato.SetEstado(new EstadoContratado());
        else if (nuevoEstado == "RECHAZADO")
            candidato.SetEstado(new EstadoRechazado());
        else
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
    }
}

public class EstadoContratado : IEstadoCandidato
{
    public string Nombre => "CONTRATADO";

    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        throw new Exception("El candidato ya fue contratado, no puede avanzar más.");
    }
}

public class EstadoRechazado : IEstadoCandidato
{
    public string Nombre => "RECHAZADO";

    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        throw new Exception("Un candidato rechazado no puede cambiar de estado.");
    }
}

public class Candidato
{
    public string Nombre { get; set; }
    public string ReclutadorEmail { get; set; }    
    public IEstadoCandidato EstadoActual { get; private set; }    
    private List<IObservador> observadores = new List<IObservador>();

    public Candidato(string nombre, string email)
    {
        Nombre = nombre;
        ReclutadorEmail = email;
        EstadoActual = new EstadoAplicado(); 
    }

    public void AgregarObservador(IObservador obs)
    {
        observadores.Add(obs);
    }

    public void IntentarAvanzar(string nuevoEstadoStr)
    {
        EstadoActual.Avanzar(this, nuevoEstadoStr);
    }

    public void SetEstado(IEstadoCandidato nuevoEstado)
    {
        string nombreAnterior = EstadoActual.Nombre;        
        
        EstadoActual = nuevoEstado;        
        
        foreach (var obs in observadores)
        {
            obs.Actualizar(this, nombreAnterior, EstadoActual.Nombre);
        }
    }

    public MementoCandidato CrearCopiaDeSeguridad()
    {
        return new MementoCandidato(EstadoActual);
    }
    
    public void Restaurar(MementoCandidato memento)
    {
        EstadoActual = memento.EstadoGuardado;
        Console.WriteLine($"[Sistema]: Se restauró a {Nombre} a la etapa {EstadoActual.Nombre}.");
    }
}

public class GestorDeCandidato
{
    private Stack<MementoCandidato> historial = new Stack<MementoCandidato>();
    
    private List<string> registroAuditoria = new List<string>();

    public void AvanzarEstado(Candidato candidato, string nuevoEstado, string usuario)
    {
        MementoCandidato fotografia = candidato.CrearCopiaDeSeguridad();
        historial.Push(fotografia);

        try
        {
            candidato.IntentarAvanzar(nuevoEstado);
            
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            string log = $"Usuario '{usuario}' cambió a {candidato.Nombre} a {nuevoEstado} el {fecha}";
            registroAuditoria.Add(log);
        }
        catch (Exception ex)
        {
            historial.Pop();
            Console.WriteLine(ex.Message);
        }
    }

    public void DeshacerUltimoCambio(Candidato candidato, string usuario)
    {
        if (historial.Count > 0)
        {
            MementoCandidato ultimaFoto = historial.Pop();
            candidato.Restaurar(ultimaFoto);
            
            string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            registroAuditoria.Add($"Usuario '{usuario}' deshizo un cambio el {fecha}");
        }
        else
        {
            Console.WriteLine("No hay cambios en el historial para deshacer.");
        }
    }

    public void ImprimirAuditoria()
    {
        Console.WriteLine("\n--- REGISTRO DE AUDITORÍA ---");
        foreach (var registro in registroAuditoria)
        {
            Console.WriteLine(registro);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        GestorDeCandidato gestor = new GestorDeCandidato();
        Candidato candidato = new Candidato("Ana Gómez", "reclutador@empresa.com");

        candidato.AgregarObservador(new ObservadorReclutador());
        candidato.AgregarObservador(new ObservadorGerente());
        candidato.AgregarObservador(new ObservadorNomina());
        candidato.AgregarObservador(new ObservadorPortal());

        Console.WriteLine("--- INTENTANDO AVANZAR A ENTREVISTA ---");
        gestor.AvanzarEstado(candidato, "ENTREVISTA", "Usuario_RRHH");

        Console.WriteLine("\n--- INTENTANDO AVANZAR A PRUEBA TÉCNICA ---");
        gestor.AvanzarEstado(candidato, "PRUEBA TECNICA", "Usuario_RRHH");

        Console.WriteLine("\n--- INTENTANDO AVANZAR A OFERTA ---");
        gestor.AvanzarEstado(candidato, "OFERTA", "Usuario_RRHH");

        Console.WriteLine("\n--- INTENTANDO AVANZAR A VERIFICACIÓN DE REFERENCIAS ---");
        gestor.AvanzarEstado(candidato, "VERIFICACION DE REFERENCIAS", "Usuario_RRHH");

        Console.WriteLine("\n--- SIMULANDO ERROR: RECHAZO ACCIDENTAL ---");
        gestor.AvanzarEstado(candidato, "RECHAZADO", "Usuario_RRHH");

        Console.WriteLine("\n--- DESHACIENDO ERROR (MEMENTO) ---");
        gestor.DeshacerUltimoCambio(candidato, "Admin_Sistema");

        Console.WriteLine("\n--- INTENTANDO AVANZAR A CONTRATADO ---");
        gestor.AvanzarEstado(candidato, "CONTRATADO", "Usuario_RRHH");

        gestor.ImprimirAuditoria();
    }
}