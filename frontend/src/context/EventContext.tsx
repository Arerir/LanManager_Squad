import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';

const STORAGE_KEY = 'selectedEventId';

interface EventContextValue {
  selectedEventId: string | null;
  setSelectedEventId: (id: string | null) => void;
}

const EventContext = createContext<EventContextValue | null>(null);

export function EventProvider({ children }: { children: ReactNode }) {
  const [selectedEventId, setSelectedEventIdState] = useState<string | null>(
    () => localStorage.getItem(STORAGE_KEY)
  );

  const setSelectedEventId = useCallback((id: string | null) => {
    setSelectedEventIdState(id);
    if (id) {
      localStorage.setItem(STORAGE_KEY, id);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, []);

  return (
    <EventContext.Provider value={{ selectedEventId, setSelectedEventId }}>
      {children}
    </EventContext.Provider>
  );
}

export function useEventContext(): EventContextValue {
  const ctx = useContext(EventContext);
  if (!ctx) throw new Error('useEventContext must be used within EventProvider');
  return ctx;
}
