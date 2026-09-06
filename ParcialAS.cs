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
        Console.WriteLine($"[Email Reclutador]: El candidato {candidato.Nombre} pasó de {estadoAnterior} a {estadoNuevo}.");
    }
}

public class ObservadorGerente : IObservador
{
    public void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo)
    {
        if (estadoNuevo == "OFERTA" || estadoNuevo == "CONTRATADO")
        {
            Console.WriteLine($"[Email Gerente]: Atención, {candidato.Nombre} ha llegado a la etapa de {estadoNuevo}.");
        }
    }
}

public class ObservadorNomina : IObservador
{
    public void Actualizar(Candidato candidato, string estadoAnterior, string estadoNuevo)
    {
        if (estadoNuevo == "CONTRATADO")
        {
            Console.WriteLine($"[Email Nomina]: El candidato, {candidato.Nombre} ha llegado a la etapa de {estadoNuevo}.");
        }
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
        {
            candidato.SetEstado(new EstadoEntrevista());
        }
        else if (nuevoEstado == "RECHAZADO")
        {
            candidato.SetEstado(new EstadoRechazado());
        }
        else
        {
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
        }
    }
}

public class EstadoEntrevista : IEstadoCandidato
{
    public string Nombre => "ENTREVISTA";
    
    public void Avanzar(Candidato candidato, string nuevoEstado)
    {        
        if (nuevoEstado == "PRUEBA TECNICA")
        { 
            candidato.SetEstado(new EstadoPruebaTecnica());
        }
        else if (nuevoEstado == "RECHAZADO")
        {
            candidato.SetEstado(new EstadoRechazado());
        }
        else
        {
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
        }
    }
}

public class EstadoPruebaTecnica : IEstadoCandidato
{
    public string Nombre => "PRUEBA TECNICA";
    
    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "OFERTA")
        {
            candidato.SetEstado(new EstadoOferta());
        }
        else if (nuevoEstado == "RECHAZADO")
        {
            candidato.SetEstado(new EstadoRechazado());
        }
        else
        {
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
        }
    }
}

public class EstadoOferta : IEstadoCandidato
{
    public string Nombre => "OFERTA";

    public void Avanzar(Candidato candidato, string nuevoEstado)
    {
        if (nuevoEstado == "CONTRATADO")
        {
            candidato.SetEstado(new EstadoContratado());
        }
        else if (nuevoEstado == "RECHAZADO")
        {
            candidato.SetEstado(new EstadoRechazado());
        }
        else
        {
            throw new Exception($"Transición inválida: de {Nombre} a {nuevoEstado}");
        }
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
        Console.WriteLine($"[Sistema]: Deshacer ejecutado. {Nombre} ha vuelto a la etapa {EstadoActual.Nombre}.");
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
                        
            string fecha = DateTime.Now.ToString("HH:mm:ss");
            registroAuditoria.Add($"El usuario {usuario} cambió al candidato a {nuevoEstado} a las {fecha}");
        }
        catch (Exception ex)
        {            
            historial.Pop();
            Console.WriteLine(ex.Message);
        }
    }
    
    public void DeshacerUltimoCambio(Candidato candidato)
    {
        if (historial.Count > 0)
        {            
            MementoCandidato ultimaFoto = historial.Pop();
            candidato.Restaurar(ultimaFoto);
            registroAuditoria.Add("Se deshizo el último cambio.");
        }
        else
        {
            Console.WriteLine("No hay cambios en el historial para deshacer.");
        }
    }
}