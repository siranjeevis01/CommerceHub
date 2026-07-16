import {
  Component,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import {
  AIAgentService,
  ChatResponse,
} from '@shared/services/ai-agent.service';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  recommendations?: ChatResponse['recommendations'];
  searchResults?: ChatResponse['searchResults'];
}

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  selector: 'app-ai-chat',
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChatComponentStandalone implements AfterViewChecked, OnDestroy {
  @ViewChild('messageList') messageListRef!: ElementRef<HTMLDivElement>;
  @ViewChild('chatInput') chatInputRef!: ElementRef<HTMLInputElement>;

  isOpen = false;
  isLoading = false;
  userInput = '';
  conversationId: number | null = null;
  messages: ChatMessage[] = [];
  errorMessage = '';

  private destroy$ = new Subject<void>();
  private shouldScroll = false;

  constructor(
    private aiAgentService: AIAgentService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.shouldScroll = true;
      setTimeout(() => this.chatInputRef?.nativeElement.focus(), 100);
    }
  }

  sendMessage(): void {
    const text = this.userInput.trim();
    if (!text || this.isLoading) return;

    this.messages.push({
      role: 'user',
      content: text,
      timestamp: new Date(),
    });
    this.userInput = '';
    this.isLoading = true;
    this.errorMessage = '';
    this.shouldScroll = true;
    this.cdr.markForCheck();

    const request = {
      message: text,
      conversationId: this.conversationId ?? undefined,
    };

    this.aiAgentService
      .chat(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const data = res.data;
          if (data) {
            this.conversationId = data.conversationId;
            this.messages.push({
              role: 'assistant',
              content: data.reply,
              timestamp: new Date(),
              recommendations: data.recommendations,
              searchResults: data.searchResults,
            });
          }
          this.isLoading = false;
          this.shouldScroll = true;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.errorMessage =
            err?.error?.message || 'Sorry, something went wrong. Please try again.';
          this.isLoading = false;
          this.shouldScroll = true;
          this.cdr.markForCheck();
        },
      });
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  clearChat(): void {
    this.messages = [];
    this.conversationId = null;
    this.errorMessage = '';
    this.cdr.markForCheck();
  }

  trackByIndex(_index: number, item: ChatMessage): number {
    return item.timestamp.getTime();
  }

  private scrollToBottom(): void {
    const el = this.messageListRef?.nativeElement;
    if (el) {
      el.scrollTop = el.scrollHeight;
    }
  }
}
