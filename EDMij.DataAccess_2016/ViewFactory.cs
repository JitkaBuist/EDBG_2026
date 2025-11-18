using System.Collections.Generic;
using EDMij.Presentation;
using EDMij.Screens;

namespace EDMij.Application
{
    /// <summary>
    /// Creates Views for the MainView.
    /// </summary>
    sealed class ViewFactory : IViewFactory
    {

        public IAansluitingenView CreateAansluitingenView()
        {
            IAansluitingenView view = new AansluitingenView();
            return view;
        }

        public IBackOfficeSchermenView CreateBackOfficeSchermenView()//(ISubScherm SubScherm)//(IBackOfficeSchermenView_SubScherm SubScherm)
        {
            IBackOfficeSchermenView view = new BackOfficeSchermenView();
            return view;
        }

        public IBackOfficeE_programView CreateBackOfficeE_programView()//(ISubScherm SubScherm)//(IBackOfficeSchermenView_SubScherm SubScherm)
        {
            IBackOfficeE_programView view = new BackOfficeE_programView();
            return view;
        }

        public IOrdersContractenView CreateOrdersContractenView()
        {
            IOrdersContractenView view = new OrdersContractenView();
            return view;
        }

        public IPortFolioView CreatePortfolioView()
        {
            IPortFolioView view = new PortfolioView();
            return view;
        }

        public IWerkplaatsView CreateWerkplaatsView()
        {
            IWerkplaatsView view = new WerkplaatsView();
            return view;
        }

        public IOnderhoudsSchermenView CreateOnderhoudsSchermenView()
        {
            IOnderhoudsSchermenView view = new OnderhoudsSchermenView();
            return view;
        }

        public IRelatiebeheerView CreateRelatiebeheerView()
        {
            IRelatiebeheerView view = new RelatiebeheerView();
            return view;
        }

        public IMainView CreateMainView()
        {
            IMainView view = new MainView();
            view.Initialize(this);
            return view;
        }

        public IInOntwikkelingView CreateInOntwikkelingView()
        {
            IInOntwikkelingView view = new InOntwikkelingView();
            return view;
        }

        public IVisualSurface CreateReportsView()
        {
            return new PlaceholderView();
        }



    
    }
}