import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

interface ComingSoonData {
  title?: string;
  description?: string;
  meta?: string;
}

@Component({
  selector: 'app-coming-soon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="coming-soon">
      <h2>{{ title }}</h2>
      <p>{{ description }}</p>
      <span class="meta">{{ meta }}</span>
    </section>
  `,
  styles: `
    .coming-soon {
      background: rgba(226, 232, 240, 0.45);
      border: 1px dashed rgba(148, 163, 184, 0.7);
      border-radius: 20px;
      padding: 3rem 2.5rem;
      text-align: center;
      display: grid;
      gap: 1rem;
      place-items: center;
      color: var(--surface-700);
    }

    .coming-soon h2 {
      margin: 0;
      font-size: 1.75rem;
      color: var(--surface-900);
    }

    .coming-soon p {
      margin: 0;
      max-width: 540px;
    }

    .coming-soon .meta {
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--surface-500);
    }
  `
})
export class ComingSoonComponent implements OnInit {
  title = 'Em construção';
  description = 'Estamos finalizando os detalhes desta área. Em breve você poderá acompanhar relatórios e tomar decisões baseada em dados.';
  meta = 'Previsto para o próximo sprint';

  constructor(private readonly route: ActivatedRoute) {}

  ngOnInit(): void {
    const data = this.route.snapshot.data['comingSoon'] as ComingSoonData | undefined;
    if (!data) {
      return;
    }

    this.title = data.title ?? this.title;
    this.description = data.description ?? this.description;
    this.meta = data.meta ?? this.meta;
  }
}
