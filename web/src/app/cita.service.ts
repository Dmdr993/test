import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

export interface Cita {
  id: number;
  pacienteId: number;
  pacienteNombre: string;
  cubiculoId: number;
  fechaHoraInicio: string;
  fechaHoraFin: string;
  estado: 'PROGRAMADA' | 'ATENDIDA' | 'CANCELADA';
}

export interface CrearCita {
  pacienteId: number;
  cubiculoId: number;
  fechaHoraInicio: string;
}

export interface Paciente { id: number; nombres: string; dni: string; }
export interface Cubiculo { id: number; nombre: string; sede: string; }
export interface CrearPaciente { nombres: string; dni: string; telefono: string | null; }
export interface CrearCubiculo { nombre: string; sede: string; }

export class ApiError extends Error {
  constructor(public readonly status: number, message: string) { super(message); }
}

@Injectable({ providedIn: 'root' })
export class CitaService {
  private readonly api = '/api';

  constructor(private readonly http: HttpClient) {}

  listar(cubiculoId: number, fecha: string): Observable<Cita[]> {
    const params = new HttpParams().set('cubiculoId', cubiculoId).set('fecha', fecha);
    return this.http.get<Cita[]>(`${this.api}/citas`, { params }).pipe(catchError(this.error));
  }

  registrar(dto: CrearCita): Observable<Cita> {
    return this.http.post<Cita>(`${this.api}/citas`, dto).pipe(catchError(this.error));
  }

  cancelar(id: number): Observable<void> {
    return this.http.patch<void>(`${this.api}/citas/${id}/cancelar`, {}).pipe(catchError(this.error));
  }

  pacientes(): Observable<Paciente[]> {
    return this.http.get<Paciente[]>(`${this.api}/pacientes`).pipe(catchError(this.error));
  }

  cubiculos(): Observable<Cubiculo[]> {
    return this.http.get<Cubiculo[]>(`${this.api}/cubiculos`).pipe(catchError(this.error));
  }

  crearPaciente(dto: CrearPaciente): Observable<Paciente> {
    return this.http.post<Paciente>(`${this.api}/pacientes`, dto).pipe(catchError(this.error));
  }

  crearCubiculo(dto: CrearCubiculo): Observable<Cubiculo> {
    return this.http.post<Cubiculo>(`${this.api}/cubiculos`, dto).pipe(catchError(this.error));
  }

  private readonly error = (response: HttpErrorResponse) => {
    const message = response.error?.detail ?? 'No se pudo completar la operación.';
    return throwError(() => new ApiError(response.status, message));
  };
}

