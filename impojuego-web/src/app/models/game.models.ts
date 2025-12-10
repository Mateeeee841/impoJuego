// === API Response wrapper ===
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

// === Player ===
export interface Player {
  name: string;
  isEliminated: boolean;
}

// === Lobby ===
export interface LobbyStatus {
  players: Player[];
  minPlayers: number;
  maxPlayers: number;
  canStart: boolean;
}

// === Game Start ===
export interface GameStarted {
  category: string;
  impostorCount: number;
  players: string[];
}

// === Player Role (reveal) ===
export interface PlayerRole {
  playerName: string;
  role: 'Impostor' | 'Crewmate';
  category: string;
  secretWord: string | null;
  fellowImpostors: string[] | null;
}

// === Game State ===
export type GamePhase = 'Lobby' | 'RoleReveal' | 'Discussion' | 'Voting' | 'Finished';

export interface GameState {
  phase: GamePhase;
  roundNumber: number;
  category: string;
  activePlayers: Player[];
  impostorCount: number;
}

// === Voting ===
export interface VoteResult {
  eliminatedPlayer: string | null;
  wasTie: boolean;
  voteCounts: { [playerName: string]: number };
  skipVotes: number;
}

export interface RoundResult {
  voteResult: VoteResult;
  gameStatus: 'InProgress' | 'ImpostorsWin' | 'CrewmatesWin';
  message: string;
}

export interface VotingStatus {
  votesCast: number;
  totalVoters: number;
  allVotesIn: boolean;
}

// === Game End ===
export interface GameEnd {
  winner: 'Crewmates' | 'Impostores';
  impostors: string[];
  secretWord: string;
}

// === Categories ===
export interface Category {
  name: string;
  wordCount: number;
}
