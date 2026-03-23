import { DOCUMENT } from '@angular/common';
import { Inject, Injectable, NgZone } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ChatNotificationService {
  private audioContext: AudioContext | null = null;
  private audioUnlocked = false;
  private unlockRegistered = false;

  constructor(
    @Inject(DOCUMENT) private readonly document: Document,
    private readonly ngZone: NgZone
  ) {
    this.registerUnlockListeners();
  }

  playIncomingMessage(): void {
    const context = this.ensureAudioContext();
    if (!context || !this.audioUnlocked || context.state !== 'running') {
      return;
    }

    const now = context.currentTime;
    const oscillator = context.createOscillator();
    const gain = context.createGain();

    oscillator.type = 'sine';
    oscillator.frequency.setValueAtTime(880, now);
    oscillator.frequency.exponentialRampToValueAtTime(660, now + 0.12);

    gain.gain.setValueAtTime(0.0001, now);
    gain.gain.exponentialRampToValueAtTime(0.05, now + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.16);

    oscillator.connect(gain);
    gain.connect(context.destination);
    oscillator.start(now);
    oscillator.stop(now + 0.18);
  }

  private registerUnlockListeners(): void {
    if (this.unlockRegistered) {
      return;
    }

    const windowRef = this.document.defaultView;
    if (!windowRef || typeof windowRef.AudioContext === 'undefined') {
      return;
    }

    this.unlockRegistered = true;

    const unlockAudio = () => {
      const context = this.ensureAudioContext();
      if (!context) {
        return;
      }

      void context.resume().then(() => {
        this.audioUnlocked = context.state === 'running';
        if (this.audioUnlocked) {
          windowRef.removeEventListener('pointerdown', unlockAudio, true);
          windowRef.removeEventListener('keydown', unlockAudio, true);
        }
      });
    };

    this.ngZone.runOutsideAngular(() => {
      windowRef.addEventListener('pointerdown', unlockAudio, true);
      windowRef.addEventListener('keydown', unlockAudio, true);
    });
  }

  private ensureAudioContext(): AudioContext | null {
    if (this.audioContext) {
      return this.audioContext;
    }

    const windowRef = this.document.defaultView;
    if (!windowRef || typeof windowRef.AudioContext === 'undefined') {
      return null;
    }

    this.audioContext = new windowRef.AudioContext();
    return this.audioContext;
  }
}
