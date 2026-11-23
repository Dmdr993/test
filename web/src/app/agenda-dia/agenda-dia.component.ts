import { ChangeDetectionStrategy, Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ApiError, Cita, CitaService, Cubiculo, Paciente } from '../cita.service';

interface Bloque { hora: string; cita?: Cita; }

@Component({
  selector: 'app-agenda-dia',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './agenda-dia.component.html',
  styleUrl: './agenda-dia.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AgendaDiaComponent implements OnInit {
  readonly hoy = this.fechaLocal(new Date());
  readonly citas = signal<Cita[]>([]);
  readonly pacientes = signal<Paciente[]>([]);
  readonly cubiculos = signal<Cubiculo[]>([]);
  readonly cargando = signal(false);
  readonly mensaje = signal('');
  readonly error = signal('');

  readonly filtro = this.fb.nonNullable.group({
    cubiculoId: [0, [Validators.required, Validators.min(1)]],
    fecha: [this.hoy, Validators.required]
  });

  readonly formulario = this.fb.nonNullable.group({
    pacienteId: [0, [Validators.required, Validators.min(1)]],
    hora: ['', Validators.required]
  });

  readonly pacienteForm = this.fb.nonNullable.group({
    nombres: ['', [Validators.required, Validators.maxLength(100)]],
    dni: ['', [Validators.required, Validators.pattern(/^\d{8}$/)]],
    telefono: ['', Validators.maxLength(15)]
  });

  readonly cubiculoForm = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.maxLength(50)]],
    sede: ['', [Validators.required, Validators.maxLength(50)]]
  });

  readonly bloques = computed<Bloque[]>(() => {
    const resultado: Bloque[] = [];
    for (let minutos = 8 * 60; minutos < 18 * 60; minutos += 30) {
      const hora = `${String(Math.floor(minutos / 60)).padStart(2, '0')}:${String(minutos % 60).padStart(2, '0')}`;
      const delBloque = this.citas().filter(item => item.fechaHoraInicio.substring(11, 16) === hora);
      const cita = delBloque.find(item => item.estado !== 'CANCELADA') ?? delBloque[0];
      resultado.push({ hora, cita });
    }
    return resultado;
  });

  constructor(private readonly fb: FormBuilder, private readonly api: CitaService) {}

  ngOnInit(): void {
    forkJoin({ pacientes: this.api.pacientes(), cubiculos: this.api.cubiculos() }).subscribe({
      next: ({ pacientes, cubiculos }) => {
        this.pacientes.set(pacientes);
        this.cubiculos.set(cubiculos);
        if (cubiculos.length) {
          this.filtro.controls.cubiculoId.setValue(cubiculos[0].id);
          this.cargar();
        }
      },
      error: error => this.mostrarError(error)
    });
  }

  cargar(): void {
    if (this.filtro.invalid) return;
    this.cargando.set(true);
    this.error.set('');
    const { cubiculoId, fecha } = this.filtro.getRawValue();
    this.api.listar(cubiculoId, fecha).subscribe({
      next: citas => { this.citas.set(citas); this.cargando.set(false); },
      error: error => { this.cargando.set(false); this.mostrarError(error); }
    });
  }

  registrar(): void {
    this.mensaje.set('');
    this.error.set('');
    if (this.formulario.invalid || this.filtro.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }
    const filtro = this.filtro.getRawValue();
    const form = this.formulario.getRawValue();
    if (form.hora < '08:00' || form.hora >= '18:00') {
      this.error.set('La hora debe estar entre las 08:00 y las 17:30.');
      return;
    }
    const fechaHoraInicio = `${filtro.fecha}T${form.hora}:00`;
    if (new Date(fechaHoraInicio) <= new Date()) {
      this.error.set('La fecha y hora deben ser futuras.');
      return;
    }
    this.api.registrar({
      pacienteId: form.pacienteId,
      cubiculoId: filtro.cubiculoId,
      fechaHoraInicio
    }).subscribe({
      next: () => {
        this.mensaje.set('Cita registrada correctamente.');
        this.formulario.reset({ pacienteId: 0, hora: '' });
        this.cargar();
      },
      error: error => this.mostrarError(error)
    });
  }

  crearPaciente(): void {
    this.error.set('');
    this.mensaje.set('');
    if (this.pacienteForm.invalid) {
      this.pacienteForm.markAllAsTouched();
      return;
    }
    const form = this.pacienteForm.getRawValue();
    this.api.crearPaciente({ ...form, telefono: form.telefono || null }).subscribe({
      next: paciente => {
        this.pacientes.update(items => [...items, paciente].sort((a, b) => a.nombres.localeCompare(b.nombres)));
        this.formulario.controls.pacienteId.setValue(paciente.id);
        this.pacienteForm.reset({ nombres: '', dni: '', telefono: '' });
        this.mensaje.set('Paciente creado correctamente.');
      },
      error: error => this.mostrarError(error)
    });
  }

  crearCubiculo(): void {
    this.error.set('');
    this.mensaje.set('');
    if (this.cubiculoForm.invalid) {
      this.cubiculoForm.markAllAsTouched();
      return;
    }
    this.api.crearCubiculo(this.cubiculoForm.getRawValue()).subscribe({
      next: cubiculo => {
        this.cubiculos.update(items => [...items, cubiculo]);
        this.filtro.controls.cubiculoId.setValue(cubiculo.id);
        this.cubiculoForm.reset({ nombre: '', sede: '' });
        this.mensaje.set('Cubículo creado correctamente.');
        this.cargar();
      },
      error: error => this.mostrarError(error)
    });
  }

  private mostrarError(error: unknown): void {
    if (error instanceof ApiError && error.status === 409) {
      this.error.set('El cubículo ya tiene una cita en ese horario. Elija otro bloque.');
    } else {
      this.error.set(error instanceof Error ? error.message : 'Ocurrió un error inesperado.');
    }
  }

  private fechaLocal(fecha: Date): string {
    const offset = fecha.getTimezoneOffset() * 60_000;
    return new Date(fecha.getTime() - offset).toISOString().substring(0, 10);
  }
}

