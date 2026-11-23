import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AgendaDiaComponent } from './agenda-dia/agenda-dia.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [AgendaDiaComponent],
  template: '<app-agenda-dia />',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {}

