import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  HostBinding,
  Input,
  ViewChild,
  ElementRef
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-rich-text-editor',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rich-text-editor.component.html',
  styleUrl: './rich-text-editor.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RichTextEditorComponent),
      multi: true
    }
  ]
})
export class RichTextEditorComponent implements ControlValueAccessor {
  @Input() placeholder = 'Digite aqui...';
  @ViewChild('editable', { static: true }) editableRef!: ElementRef<HTMLDivElement>;

  value = '';
  disabled = false;

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(obj: string | null): void {
    this.value = obj ?? '';
    if (this.editableRef) {
      this.editableRef.nativeElement.innerHTML = this.value || '';
    }
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  handleInput(): void {
    if (!this.editableRef) {
      return;
    }
    this.value = this.editableRef.nativeElement.innerHTML;
    this.onChange(this.value);
  }

  handleBlur(): void {
    this.onTouched();
  }

  applyFormat(command: string): void {
    if (this.disabled || typeof document === 'undefined') {
      return;
    }

    document.execCommand(command, false);
    this.handleInput();
    this.editableRef.nativeElement.focus();
  }

  clearFormatting(): void {
    if (this.disabled || typeof document === 'undefined') {
      return;
    }
    document.execCommand('removeFormat', false);
    this.handleInput();
  }
}
