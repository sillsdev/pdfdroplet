import { Modal } from "./Modal";

export type AboutDialogProps = {
  isOpen: boolean;
  onClose: () => void;
};

export function AboutDialog({ isOpen, onClose }: AboutDialogProps) {
  
  return (
    <Modal isOpen={isOpen} onClose={onClose} >
      <div className="flex flex-col items-center gap-6 text-center">
        <img
          src="/images/sil-logo.png"
          alt="SIL International Logo"
          className="h-32 w-auto object-contain"
          data-testid="sil-logo"
        />
        
          <h3 className="text-2xl font-bold text-droplet-primary">
            PDF Droplet
          </h3>
        
                
            <a
              href="https://software.sil.org/pdfdroplet/"
              target="_blank"
              rel="noopener noreferrer"
              className="text-droplet-accent hover:underline"
              data-testid="pdfdroplet-link"
            >
              software.sil.org/pdfdroplet
            </a>
          
        <div className="mt-4 space-y-1 text-sm text-slate-600">
          <p>Copyright © 2012-2025 SIL Global</p>
          <p className="text-xs text-slate-500">
            Licensed under the MIT License
          </p>
        </div>
        <button
          type="button"
          onClick={onClose}
          className="mt-4 rounded-lg bg-droplet-accent px-6 py-2 text-white transition-colors hover:bg-droplet-accent/90 focus:outline-none focus:ring-2 focus:ring-droplet-accent focus:ring-offset-2"
          data-testid="about-close-button"
        >
          Close
        </button>
      </div>
    </Modal>
  );
}
